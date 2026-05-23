import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FlightCardComponent } from './flight-card.component';
import { FlightResult, SortOption } from '../models/flight.models';

@Component({
  selector: 'app-flight-results',
  standalone: true,
  imports: [CommonModule, FlightCardComponent],
  templateUrl: './flight-results.component.html',
  styleUrl: './flight-results.component.scss'
})
export class FlightResultsComponent {
  private readonly _flights = signal<FlightResult[]>([]);
  @Input() set flights(value: FlightResult[]) { this._flights.set(value ?? []); }

  @Input() loading = false;
  @Input() searched = false;
  @Input() error: string | null = null;

  readonly sort = signal<SortOption>('priceAsc');

  readonly sortedFlights = computed<FlightResult[]>(() => {
    const list = [...this._flights()];
    switch (this.sort()) {
      case 'priceAsc':     return list.sort((a, b) => a.pricePerPassenger - b.pricePerPassenger);
      case 'priceDesc':    return list.sort((a, b) => b.pricePerPassenger - a.pricePerPassenger);
      case 'durationAsc':  return list.sort((a, b) => this.toMinutes(a.duration) - this.toMinutes(b.duration));
      case 'departureAsc': return list.sort((a, b) => new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime());
    }
  });

  readonly sortOptions: Array<{ value: SortOption; label: string }> = [
    { value: 'priceAsc',     label: 'Price (low → high)'   },
    { value: 'priceDesc',    label: 'Price (high → low)'   },
    { value: 'durationAsc',  label: 'Duration (shortest)'  },
    { value: 'departureAsc', label: 'Departure (earliest)' }
  ];

  setSort(value: SortOption): void {
    this.sort.set(value);
  }

  private toMinutes(duration: string): number {
    const [h = '0', m = '0'] = duration.split(':');
    return parseInt(h, 10) * 60 + parseInt(m, 10);
  }
}
