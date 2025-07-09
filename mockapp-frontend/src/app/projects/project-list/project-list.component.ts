import { Component, OnInit } from '@angular/core';
import { Project } from '../../../services/models/project.model';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ProjectApiService } from '../../../services/apis/project-api.service';
import { ProjectService } from '../../../services/project.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import {
  CreateOrUpdateProjectComponent,
  CreateOrUpdateProjectDialogData,
} from '../create-or-update-project/create-or-update-project.component';
import { finalize } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss'],
  imports: [
    RouterModule,
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatIconModule,
    MatMenuModule,
    TranslateModule,
  ],
})
export class ProjectListComponent implements OnInit {
  projects: Project[] = [];
  loading: boolean = true;
  columns: string[] = ['name', 'secret', 'createdAt', 'actions'];

  constructor(
    private projectApiService: ProjectApiService,
    private projectService: ProjectService,
    private router: Router,
    private dialog: MatDialog,
    private snackbarService: SnackbarService,
    private translate: TranslateService
  ) {}

  private refreshProjects() {
    this.loading = true;
    this.projectApiService
      .getAll()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (projects) => (this.projects = projects),
        error: (err) =>
          this.snackbarService.show({
            message: this.translate.instant(
              'WYSTPI_BD_PODCZAS_POBIERANIA_PROJEKTW'
            ),
            type: 'error',
          }),
      });
  }

  createProject() {
    this.openCreateOrUpdateDialog();
  }

  editProject(project: Project) {
    this.openCreateOrUpdateDialog(project);
  }

  goToMocks(project: Project) {
    this.projectService.setCurrentProject(project);
    this.router.navigate(['/projects', project.id, 'mocks']);
  }

  copySecret(secret: string) {
    navigator.clipboard.writeText(secret);
  }

  deleteProject(project: Project) {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
        ConfirmDialogComponent,
        {
          data: {
            title: 'POTWIERDZENIE_USUNIECIA',
            message: 'CZY_NA_PEWNO_USUNAC_PROJEKT',
            params: {
              project: project.name,
            },
            confirmText: 'USUN',
            cancelText: 'ANULUJ',
          },
        }
      )
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.projectApiService.delete(project.id).subscribe(() => {
            this.refreshProjects();
          });
        }
      });
  }

  private openCreateOrUpdateDialog(project?: Project) {
    const dialogRef = this.dialog.open<
      CreateOrUpdateProjectComponent,
      CreateOrUpdateProjectDialogData,
      Project
    >(CreateOrUpdateProjectComponent, {
      width: '400px',
      data: { project },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.refreshProjects();
      }
    });
  }

  ngOnInit() {
    this.projectService.clearCurrentProject();
    this.refreshProjects();
  }
}
