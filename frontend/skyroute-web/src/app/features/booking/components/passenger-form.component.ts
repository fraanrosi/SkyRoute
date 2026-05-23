import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-passenger-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './passenger-form.component.html',
  styleUrl: './passenger-form.component.scss'
})
export class PassengerFormComponent {
  @Input({ required: true }) group!: FormGroup;
  @Input({ required: true }) index!: number;
  @Input({ required: true }) documentLabel!: string;
  @Input({ required: true }) documentHint!: string;
  @Input({ required: true }) isInternational!: boolean;

  get documentControl() {
    return this.group.get('documentNumber');
  }

  documentError(): string | null {
    const ctrl = this.documentControl;
    if (!ctrl || !ctrl.touched || ctrl.valid) return null;
    if (ctrl.hasError('required')) return 'Required.';
    if (ctrl.hasError('passport')) return 'Passport must be 6–9 uppercase letters or digits.';
    if (ctrl.hasError('nationalId')) return 'National ID must be 7–8 digits.';
    return 'Invalid.';
  }
}
