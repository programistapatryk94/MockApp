import {
  Component,
  ElementRef,
  Inject,
  OnInit,
  ViewChild,
} from '@angular/core';
import {
  AddProjectMemberInput,
  ProjectMemberDto,
} from '../../../services/models/project-member.model';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { Project } from '../../../services/models/project.model';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { ProjectMemberApiService } from '../../../services/apis/project-member-api.service';
import { catchError, EMPTY, finalize } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

type CreateProjectMemberFormModel = {
  email: string;
};

export interface ProjectMembersDialogData {
  project: Project;
}

@Component({
  selector: 'app-project-members-dialog',
  templateUrl: './project-members-dialog.component.html',
  styleUrls: ['./project-members-dialog.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatDialogModule,
    MatInputModule,
    SpinnerContentComponent,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
})
export class ProjectMembersDialogComponent implements OnInit {
  members: ProjectMemberDto[] = [];
  loading: boolean = false;
  form!: FormGroup<TypedFormControls<CreateProjectMemberFormModel>>;
  private _saving = false;
  @ViewChild('emailInput') emailInputRef!: ElementRef;

  get saving(): boolean {
    return this._saving;
  }

  set saving(value: boolean) {
    this._saving = value;

    if (value) {
      this.form.disable();
    } else {
      this.form.enable();
    }
  }

  get emailControl() {
    return this.form.controls.email;
  }

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ProjectMembersDialogComponent>,
    private projectMembersApiService: ProjectMemberApiService,
    private snackbarService: SnackbarService,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ProjectMembersDialogData
  ) {}

  addMember() {
    if (this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;

    const dto: AddProjectMemberInput = {
      email: this.form.value.email!,
    };

    this.projectMembersApiService
      .create(this.data.project.id, dto)
      .pipe(
        catchError((err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
          return EMPTY;
        }),
        finalize(() => (this.saving = false))
      )
      .subscribe((member) => {
        this.form.reset();
        this.emailInputRef.nativeElement.focus();
        this.snackbarService.show({
          message: this.translate.instant('DODANO_CZLONKA'),
          type: 'success',
        });
        this.loadMembers();
      });
  }

  removeMember(userId: string) {
    this.projectMembersApiService
      .delete(this.data.project.id, userId)
      .pipe(
        catchError((err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.snackbarService.show({
          message: this.translate.instant('USUNIETO_CZLONKA'),
          type: 'success',
        });
        this.loadMembers();
      });
  }

  loadMembers() {
    this.loading = true;
    this.projectMembersApiService
      .getAll(this.data.project.id)
      .pipe(
        catchError((err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
          return EMPTY;
        }),
        finalize(() => (this.loading = false))
      )
      .subscribe((members) => {
        this.members = members;
      });
  }

  ngOnInit(): void {
    this.loadMembers();
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    }) as FormGroup<TypedFormControls<CreateProjectMemberFormModel>>;
  }
}
