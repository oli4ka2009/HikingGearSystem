import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { TripService, Trip } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TripDialogComponent } from '../trip-dialog/trip-dialog.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  providers: [DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly tripService = inject(TripService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  trips = signal<Trip[]>([]);

  ngOnInit(): void {
    this.loadTrips();
  }

  loadTrips(): void {
    this.tripService.getAll().subscribe({
      next: (data) => this.trips.set(data),
      error: (err) => console.error('Помилка при завантаженні походів', err)
    });
  }

  createTrip(): void {
    const dialogRef = this.dialog.open(TripDialogComponent, {
      width: '450px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.tripService.createTrip(result).subscribe({
          next: () => {
            this.snackBar.open('Подорож успішно створена', 'Закрити', { duration: 3000 });
            this.loadTrips();
          },
          error: (err) => {
            console.error('Помилка при створенні подорожі', err);
            this.snackBar.open('Помилка при створенні подорожі', 'Закрити', { duration: 3000 });
          }
        });
      }
    });
  }

  deleteTrip(id: number): void {
    if (window.confirm('Точно видалити цей похід разом з усім спорядженням?')) {
      this.tripService.deleteTrip(id).subscribe({
        next: () => {
          this.snackBar.open('Подорож видалена', 'Закрити', { duration: 3000 });
          this.loadTrips();
        },
        error: (err) => {
          console.error('Помилка при видаленні подорожі', err);
          this.snackBar.open('Помилка при видаленні подорожі', 'Закрити', { duration: 3000 });
        }
      });
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
