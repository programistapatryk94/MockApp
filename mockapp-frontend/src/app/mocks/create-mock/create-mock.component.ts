import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { MockApiService } from '../../../services/apis/mock-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { noWhitespaceValidator } from '../../../validators/no-whitespace.validator';
import { ProjectApiService } from '../../../services/apis/project-api.service';
import { ProjectService } from '../../../services/project.service';
import { Project } from '../../../services/models/project.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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
export class CreateMockComponent implements OnInit {
  form: FormGroup<TypedFormControls<CreateMockFormModel>>;
  error: string = '';
  submitted: boolean = false;
  project: Project | null = null;

  constructor(
    private fb: FormBuilder,
    private mockApiService: MockApiService,
    private router: Router,
    private projectApiService: ProjectApiService,
    private projectService: ProjectService,
    private destroyRef: DestroyRef,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      urlPath: ['', [Validators.required, noWhitespaceValidator()]],
      method: ['GET', Validators.required],
      statusCode: [200, Validators.required],
      responseBody: ['{}', [Validators.required, noWhitespaceValidator()]],
      headersJson: [''],
    });
  }

  submit() {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value;

    this.mockApiService
      .createMock({
        method: value.method!,
        responseBody: value.responseBody!,
        statusCode: value.statusCode!,
        urlPath: value.urlPath!,
        headersJson: value.headersJson || undefined,
        projectId: this.project?.id || ''
      })
      .subscribe({
        next: () => this.router.navigate(['/mocks']),
        error: (err) => (this.error = 'Błąd przy zapisie mocka'),
      });
  }

  ngOnInit(): void {
    this.projectService
      .getCurrentProject()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((project) => {
        this.project = project;
      });

    if (!this.project) {
      const id = this.route.snapshot.paramMap.get('id');

      if (id) {
        this.projectApiService.get(id).subscribe({
          next: (p) => {
            this.projectService.setCurrentProject(p);
          },
          error: () => this.projectService.clearCurrentProject(),
        });
      }
    }
  }
}
