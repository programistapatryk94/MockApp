import { Component, DestroyRef, OnInit } from '@angular/core';
import { CreateMockInput, Mock } from '../../../services/models/mock.model';
import { MockApiService } from '../../../services/apis/mock-api.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { Project } from '../../../services/models/project.model';
import { ProjectApiService } from '../../../services/apis/project-api.service';
import { ProjectService } from '../../../services/project.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog } from '@angular/material/dialog';
import {
  CreateOrUpdateMockComponent,
  CreateOrUpdateMockDialogData,
} from '../create-or-update-mock/create-or-update-mock.component';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-mock-list',
  templateUrl: './mock-list.component.html',
  styleUrls: ['./mock-list.component.scss'],
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
    MatMenuModule
  ],
})
export class MockListComponent implements OnInit {
  mocks: Mock[] = [];
  loading: boolean = true;
  error: string = '';
  columns: string[] = [
    'method',
    'urlPath',
    'statusCode',
    'createdAt',
    'actions',
  ];
  project: Project | null = null;

  constructor(
    private mockApiService: MockApiService,
    private route: ActivatedRoute,
    private projectApiService: ProjectApiService,
    private projectService: ProjectService,
    private destroyRef: DestroyRef,
    private dialog: MatDialog
  ) {}

  createMock() {
    this.openCreateOrUpdateDialog();
  }

  editMock(mock: Mock) {
    this.openCreateOrUpdateDialog(mock);
  }

  deleteMock(mock: Mock) {}

  private refreshMocks() {
    if (null == this.project) {
      return;
    }

    this.loading = true;
    this.mockApiService.getMocks(this.project.id).subscribe({
      next: (mocks) => {
        this.mocks = mocks;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Błąd podczas pobierania mocków';
        this.loading = false;
      },
    });
  }

  private openCreateOrUpdateDialog(mock?: Mock) {
    const dialogRef = this.dialog.open<
      CreateOrUpdateMockComponent,
      CreateOrUpdateMockDialogData,
      CreateMockInput
    >(CreateOrUpdateMockComponent, {
      width: '400px',
      data: { mock, projectId: this.project!.id },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) return;
      if (mock?.id != null) {
        //TODO: Dodaj update
      } else {
        //Nowy
        this.mockApiService.createMock(result).subscribe({
          next: (createdMock) => this.refreshMocks(),
          error: (err) => {
            //TODO: Dodaj obsługe błędu
          },
        });
      }
    });
  }

  ngOnInit() {
    this.projectService
      .getCurrentProject()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((project) => {
        this.project = project;
        this.refreshMocks();
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
