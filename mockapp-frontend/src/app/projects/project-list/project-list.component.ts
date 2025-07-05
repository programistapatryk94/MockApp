import { Component, OnInit } from '@angular/core';
import {
  CreateProjectInput,
  Project,
} from '../../../services/models/project.model';
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
  ],
})
export class ProjectListComponent implements OnInit {
  projects: Project[] = [];
  loading: boolean = true;
  error: string = '';
  columns: string[] = ['name', 'secret', 'createdAt', 'actions'];

  constructor(
    private projectApiService: ProjectApiService,
    private projectService: ProjectService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  private refreshProjects() {
    this.loading = true;
    this.projectApiService.getAll().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Błąd podczas pobierania projektów';
        this.loading = false;
      },
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
    this.projectApiService.delete(project.id).subscribe({
      next: () => {
        this.refreshProjects();
      },
      error: (err) => {
        //TODO: dodaj obsługę błędu
      },
    });
  }

  private openCreateOrUpdateDialog(project?: Project) {
    const dialogRef = this.dialog.open<
      CreateOrUpdateProjectComponent,
      CreateOrUpdateProjectDialogData,
      CreateProjectInput
    >(CreateOrUpdateProjectComponent, {
      width: '400px',
      data: { project },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        if (project?.id != null) {
          //TODO: Dodaj update
        } else {
          //Nowy
          this.projectApiService.create(result).subscribe({
            next: (createdProject) => this.refreshProjects(),
            error: (err) => {
              //TODO: dodaj obsługę błędu
            },
          });
        }
      }
    });
  }

  ngOnInit() {
    this.projectService.clearCurrentProject();
    this.refreshProjects();
  }
}
