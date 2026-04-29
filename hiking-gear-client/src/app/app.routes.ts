import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TripDetailsComponent } from './components/trip-details/trip-details.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'trips/:id', component: TripDetailsComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
