import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const PASSPORT_PATTERN = /^[A-Z0-9]{6,9}$/;

export const passportValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value: string | null = control.value;
  if (!value) return null;
  const normalized = value.trim().toUpperCase();
  return PASSPORT_PATTERN.test(normalized) ? null : { passport: true };
};
