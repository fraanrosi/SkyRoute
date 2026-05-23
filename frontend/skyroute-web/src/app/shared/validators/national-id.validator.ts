import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const NATIONAL_ID_PATTERN = /^\d{7,8}$/;

export const nationalIdValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value: string | null = control.value;
  if (!value) return null;
  return NATIONAL_ID_PATTERN.test(value.trim()) ? null : { nationalId: true };
};
