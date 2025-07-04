import { Component, OnInit } from '@angular/core';
import { Mock } from '../../../services/models/mock.model';
import { MockService } from '../../../services/mock.service';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';

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
  ],
})
export class MockListComponent implements OnInit {
  mocks: Mock[] = [];
  loading: boolean = true;
  error: string = '';
  columns: string[] = ['method', 'urlPath', 'statusCode', 'createdAt', 'actions'];

  constructor(private mockService: MockService) {}

  deleteMock(mock: Mock) {
    
  }

  ngOnInit() {
    this.mockService.getMocks().subscribe({
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
}
