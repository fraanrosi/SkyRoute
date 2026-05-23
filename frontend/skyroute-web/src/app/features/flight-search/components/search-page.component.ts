import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FlightSearchService } from '../services/flight-search.service';
import { FlightResult, FlightSearchRequest } from '../models/flight.models';
import { SearchFormComponent } from './search-form.component';
import { FlightResultsComponent } from './flight-results.component';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, SearchFormComponent, FlightResultsComponent],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.scss'
})
export class SearchPageComponent {
  private readonly flightSearch = inject(FlightSearchService);

  readonly loading = signal(false);
  readonly searched = signal(false);
  readonly error = signal<string | null>(null);
  readonly flights = signal<FlightResult[]>(this.flightSearch.lastResults());

  onSearch(request: FlightSearchRequest): void {
    this.loading.set(true);
    this.error.set(null);
    this.flightSearch.search(request).subscribe({
      next: results => {
        this.flights.set(results);
        this.searched.set(true);
        this.loading.set(false);
      },
      error: err => {
        const message = err?.error?.error ?? 'Search failed. Please try again.';
        this.error.set(message);
        this.loading.set(false);
        this.searched.set(true);
        this.flights.set([]);
      }
    });
  }
}
