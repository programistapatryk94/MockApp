import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function noWhitespaceValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const isWhitespace = (control.value || '').toString().trim().length === 0;
    return isWhitespace ? { whitespace: true } : null;
  };
}
