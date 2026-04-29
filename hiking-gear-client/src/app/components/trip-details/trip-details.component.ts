import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { GearService, AiGearResponseDto } from '../../services/gear.service';

@Component({
  selector: 'app-trip-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatCardModule
  ],
  templateUrl: './trip-details.component.html',
  styleUrl: './trip-details.component.css'
})
export class TripDetailsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly gearService = inject(GearService);

  readonly tripId = this.route.snapshot.paramMap.get('id');
  isGenerating = signal<boolean>(false);
  gearList = signal<AiGearResponseDto | null>(null);

  generateGear(): void {
    if (!this.tripId) return;

    this.isGenerating.set(true);
    this.gearService.generateGearForTrip(Number(this.tripId)).subscribe({
      next: (response) => {
        this.isGenerating.set(false);
        this.gearList.set(response);
        alert('Список успішно згенеровано!');
      },
      error: (err) => {
        this.isGenerating.set(false);
        console.error('Помилка генерації:', err);
        alert('Сталася помилка при генерації списку.');
      }
    });
  }
}
