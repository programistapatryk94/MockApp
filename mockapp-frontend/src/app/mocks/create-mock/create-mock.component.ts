import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { AuthService } from '../../../helpers/auth.service';
import { MockService } from '../../../services/mock.service';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

interface CreateMockFormModel {
  urlPath: string;
  method: string;
  statusCode: number;
  responseBody: string;
  headersJson: string | null;
}

@Component({
  selector: 'app-create-mock',
  templateUrl: './create-mock.component.html',
  styleUrls: ['./create-mock.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
})
export class CreateMockComponent {
  form: FormGroup<TypedFormControls<CreateMockFormModel>>;
  error: string = '';

  constructor(
    private fb: FormBuilder,
    private mockService: MockService,
    private router: Router
  ) {
    this.form = this.fb.group({
      urlPath: ['', Validators.required],
      method: ['GET', Validators.required],
      statusCode: [200, Validators.required],
      responseBody: ['{}', Validators.required],
      headersJson: [''],
    });
  }

  submit() {
    if (this.form.invalid) return;
    const value = this.form.value;

    this.mockService
      .createMock({
        method: value.method!,
        responseBody: value.responseBody!,
        statusCode: value.statusCode!,
        urlPath: value.urlPath!,
        headersJson: value.headersJson || undefined,
      })
      .subscribe({
        next: () => this.router.navigate(['/mocks']),
        error: (err) => (this.error = 'Błąd przy zapisie mocka'),
      });
  }
}
