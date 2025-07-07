import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AuthService } from '../../../helpers/auth.service';
import { CommonModule } from '@angular/common';
import { TypedFormControls } from '../../../helpers/form';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { finalize } from 'rxjs';
import { RouterModule } from '@angular/router';

type LoginFormModel = {
  email: string;
  password: string;
};

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterModule
  ],
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  form: FormGroup<TypedFormControls<LoginFormModel>>;
  saving: boolean = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private snackbarService: SnackbarService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    }) as FormGroup<TypedFormControls<LoginFormModel>>;
  }

  submit() {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;

    const { email, password } = this.form.value;

    this.auth
      .login(email!, password!)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        error: (err) => {
          this.snackbarService.show({
            message: err.error ?? 'Nieprawidłowe dane logowania',
            type: 'error',
          });
        },
      });
  }
}
