import { Component, inject, OnInit, AfterViewInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { HttpClient } from '@angular/common/http';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { of, forkJoin } from 'rxjs';
import maplibregl from 'maplibre-gl';
import { AccommodationFormat, TripService } from '../../services/trip.service';
import { HttpErrorResponse } from '@angular/common/http';

const HOVERLA = { lat: 48.1597, lng: 24.5001, zoom: 11 };
const MAPY_API_KEY = '3o8YwpBjR3xt5Bn3xabQG9uQ0Wm2Gun9l6r0ShNWjP0';

@Component({
  selector: 'app-trip-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatIconModule,
    MatAutocompleteModule
  ],
  templateUrl: './trip-edit-dialog.component.html',
  styleUrl: './trip-edit-dialog.component.css'
})
export class TripEditDialogComponent implements OnInit, AfterViewInit, OnDestroy {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<TripEditDialogComponent>);
  private tripService = inject(TripService);
  private http = inject(HttpClient);
  public data = inject(MAT_DIALOG_DATA);

  private map!: maplibregl.Map;
  private marker?: maplibregl.Marker;
  private popup?: maplibregl.Popup;

  public AccommodationFormat = AccommodationFormat;
  serverErrors: any = {};
  isSearching = false;
  searchResults = signal<any[]>([]);

  tripForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    locationName: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    groupSize: [1, [Validators.required, Validators.min(1)]],
    accommodationFormat: [AccommodationFormat.None, Validators.required],
    latitude: [0, Validators.required],
    longitude: [0, Validators.required]
  });

  ngOnInit(): void {
    if (this.data && this.data.trip) {
      this.tripForm.patchValue(this.data.trip);
    }
  }

  ngAfterViewInit(): void {
    this.initMap();
    this.initSearchAutocomplete();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private initMap(): void {
    const lat = this.data.trip?.latitude || HOVERLA.lat;
    const lng = this.data.trip?.longitude || HOVERLA.lng;

    this.map = new maplibregl.Map({
      container: 'edit-map',
      style: this.getTileStyle(),
      center: [lng, lat],
      zoom: 12
    });

    this.map.addControl(new maplibregl.NavigationControl(), 'top-right');
    this.map.on('click', (e) => this.handleMapClick(e.lngLat.lat, e.lngLat.lng));

    // Початковий маркер
    if (this.data.trip?.latitude) {
      this.placeMarker(lng, lat, this.data.trip.locationName);
    }
  }

  private getTileStyle(): any {
    const tiles = MAPY_API_KEY
      ? [`https://api.mapy.cz/v1/maptiles/outdoor/256/{z}/{x}/{y}?apikey=${MAPY_API_KEY}`]
      : ['https://tile.opentopomap.org/{z}/{x}/{y}.png'];
    const sourceKey = MAPY_API_KEY ? 'mapy' : 'topo';
    const attribution = MAPY_API_KEY
      ? '&copy; <a href="https://mapy.cz">Mapy.cz</a>'
      : '&copy; <a href="https://opentopomap.org">OpenTopoMap</a>';
    return {
      version: 8,
      sources: { [sourceKey]: { type: 'raster', tiles, tileSize: 256, attribution } },
      layers: [{ id: sourceKey, type: 'raster', source: sourceKey }]
    };
  }

  private initSearchAutocomplete(): void {
    this.tripForm.get('locationName')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(query => {
        if (!query || typeof query !== 'string' || query.length < 3) {
          this.searchResults.set([]);
          return of([]);
        }
        return this.http.get<any[]>(
          `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&countrycodes=ua&limit=5`
        ).pipe(catchError(() => of([])));
      })
    ).subscribe(results => this.searchResults.set(results));
  }

  onLocationSelected(event: MatAutocompleteSelectedEvent): void {
    const selected = this.searchResults().find(r => r.display_name === event.option.value);
    if (selected) {
      const lat = parseFloat(selected.lat);
      const lng = parseFloat(selected.lon);
      this.placeMarker(lng, lat, selected.display_name);
      this.tripForm.patchValue({ latitude: lat, longitude: lng, locationName: selected.display_name });
      this.map.flyTo({ center: [lng, lat], zoom: 14 });
      this.searchResults.set([]);
    }
  }

  private handleMapClick(lat: number, lng: number): void {
    this.placeMarker(lng, lat, 'Шукаю...');
    this.tripForm.patchValue({ latitude: lat, longitude: lng });
    this.isSearching = true;

    const natureRadius = 2000;  // природні об'єкти — точніше, 2км
    const placeRadius = 15000; // населені пункти — до 15км
    const query = `
      [out:json][timeout:20];
      (
        node["natural"="peak"](around:${natureRadius},${lat},${lng});
        node["natural"="saddle"](around:${natureRadius},${lat},${lng});
        way["natural"="ridge"](around:${natureRadius},${lat},${lng});

        node["waterway"="waterfall"](around:${natureRadius},${lat},${lng});
        way["waterway"="waterfall"](around:${natureRadius},${lat},${lng});
        node["natural"="waterfall"](around:${natureRadius},${lat},${lng});

        node["natural"="water"](around:${natureRadius},${lat},${lng});
        way["natural"="water"](around:${natureRadius},${lat},${lng});
        relation["natural"="water"](around:${natureRadius},${lat},${lng});
        way["water"="lake"](around:${natureRadius},${lat},${lng});
        relation["water"="lake"](around:${natureRadius},${lat},${lng});
        way["water"="pond"](around:${natureRadius},${lat},${lng});
        relation["water"="pond"](around:${natureRadius},${lat},${lng});

        way["natural"="grassland"](around:${natureRadius},${lat},${lng});
        relation["natural"="grassland"](around:${natureRadius},${lat},${lng});
        way["landuse"="meadow"](around:${natureRadius},${lat},${lng});
        relation["landuse"="meadow"](around:${natureRadius},${lat},${lng});
        way["natural"="heath"](around:${natureRadius},${lat},${lng});

        node["place"~"village|hamlet|isolated_dwelling"](around:${placeRadius},${lat},${lng});
        node["tourism"="alpine_hut"](around:${natureRadius},${lat},${lng});
        node["tourism"="wilderness_hut"](around:${natureRadius},${lat},${lng});
      );
      out center tags;
    `;

    forkJoin({
      overpass: this.http.get<any>(
        `https://overpass-api.de/api/interpreter?data=${encodeURIComponent(query)}`
      ).pipe(catchError(() => of({ elements: [] }))),
      nominatim: this.http.get<any>(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=14`
      ).pipe(catchError(() => of(null)))
    }).subscribe(({ overpass, nominatim }) => {
      this.isSearching = false;
      this.buildLocationName(lat, lng, overpass, nominatim);
    });
  }

  private buildLocationName(lat: number, lng: number, overpassRes: any, nominatimRes: any): void {
    const elements = overpassRes?.elements || [];

    const getTypeKey = (el: any): string =>
      el.tags?.waterway || el.tags?.natural || el.tags?.water ||
      el.tags?.landuse || el.tags?.place || el.tags?.tourism || '';

    const isNaturalFeature = (typeKey: string) =>
      ['peak', 'saddle', 'waterfall', 'water', 'lake', 'pond',
        'grassland', 'heath', 'meadow', 'alpine_hut', 'wilderness_hut'].includes(typeKey);

    const isVillage = (typeKey: string) =>
      ['village', 'hamlet', 'isolated_dwelling'].includes(typeKey);

    const withDistance = elements
      .map((el: any) => {
        const elLat = el.lat ?? el.center?.lat;
        const elLng = el.lon ?? el.center?.lon;
        if (!elLat || !elLng) return null;
        const name = el.tags?.['name:uk'] || el.tags?.name;
        if (!name) return null;
        const typeKey = getTypeKey(el);
        return {
          ...el, elLat, elLng, name, typeKey,
          distance: this.distanceKm(lat, lng, elLat, elLng)
        };
      })
      .filter(Boolean)
      .sort((a: any, b: any) => a.distance - b.distance);

    const main = withDistance.find((el: any) => isNaturalFeature(el.typeKey))
      ?? withDistance[0];

    const nearestVillage = withDistance.find((el: any) => isVillage(el.typeKey));

    const region = nominatimRes?.address?.state
      || nominatimRes?.address?.county
      || null;

    const addr = nominatimRes?.address || {};
    const nominatimPlace =
      addr.village || addr.hamlet || addr.suburb ||
      addr.town || addr.city_district || addr.city || null;
    const country = addr.country || null;

    const parts: string[] = [];

    if (main) {
      parts.push(this.buildLabel(main));
    }

    const placeName = nominatimPlace && nominatimPlace !== main?.name
      ? nominatimPlace
      : (nearestVillage && nearestVillage !== main ? nearestVillage.name : null);
    if (placeName) {
      parts.push(placeName);
    }

    if (region && region !== placeName) {
      parts.push(region);
    }

    if (country) {
      parts.push(country);
    }

    const locationText = parts.filter(Boolean).join(', ')
      || `${lat.toFixed(4)}, ${lng.toFixed(4)}`;

    this.placeMarker(lng, lat, main ? this.buildLabel(main) : locationText);
    this.tripForm.patchValue({ locationName: locationText });
  }

  private buildLabel(el: any): string {
    const name = el.name || el.tags?.['name:uk'] || el.tags?.name || 'Без назви';
    const ele = el.tags?.ele;
    const elevationStr = ele ? ` ${ele}м` : '';
    return `${name}${elevationStr}`;
  }

  private distanceKm(lat1: number, lng1: number, lat2: number, lng2: number): number {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLng = (lng2 - lng1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2 +
      Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
      Math.sin(dLng / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  }

  private placeMarker(lng: number, lat: number, label: string): void {
    this.popup?.remove();
    if (this.marker) {
      this.marker.setLngLat([lng, lat]);
    } else {
      this.marker = new maplibregl.Marker({ color: '#e53935' })
        .setLngLat([lng, lat])
        .addTo(this.map);
    }
    this.popup = new maplibregl.Popup({ offset: 25, closeButton: false })
      .setLngLat([lng, lat])
      .setHTML(`<strong>${label}</strong>`)
      .addTo(this.map);
  }

  save(): void {
    if (this.tripForm.valid) {
      this.serverErrors = {};
      const tripData = this.tripForm.value;
      const tripId = this.data.tripId;

      this.tripService.updateTrip(tripId, tripData).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 400 && err.error?.errors) {
            this.serverErrors = err.error.errors;
            Object.keys(this.serverErrors).forEach(key => {
              const controlName = key.charAt(0).toLowerCase() + key.slice(1);
              const control = this.tripForm.get(controlName);
              if (control) {
                control.setErrors({ serverError: this.serverErrors[key][0] });
              }
            });
          }
          console.error('Помилка оновлення походу:', err);
        }
      });
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
