import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'search' },
  {
    path: 'search',
    loadComponent: () =>
      import('./features/flight-search/components/search-page.component').then(m => m.SearchPageComponent)
  },
  {
    path: 'booking/:flightId',
    loadComponent: () =>
      import('./features/booking/components/booking-page.component').then(m => m.BookingPageComponent)
  },
  {
    path: 'booking/confirmation/:reference',
    loadComponent: () =>
      import('./features/booking/components/booking-confirmation.component').then(m => m.BookingConfirmationComponent)
  },
  {
    path: 'bookings',
    loadComponent: () =>
      import('./features/booking/components/bookings-page.component').then(m => m.BookingsPageComponent)
  },
  { path: '**', redirectTo: 'search' }
];
