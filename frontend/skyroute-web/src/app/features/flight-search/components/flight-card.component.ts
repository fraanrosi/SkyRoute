import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { FlightResult } from '../models/flight.models';

@Component({
  selector: 'app-flight-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './flight-card.component.html',
  styleUrl: './flight-card.component.scss'
})
export class FlightCardComponent {
  @Input({ required: true }) flight!: FlightResult;

  get durationLabel(): string {
    const [h, m] = this.flight.duration.split(':');
    return `${parseInt(h, 10)}h ${parseInt(m, 10)}m`;
  }

  get cabinLabel(): string {
    return ['Economy', 'Business', 'First'][this.flight.cabinClass] ?? 'Economy';
  }
}
