import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddProjectMemberInput,
  ProjectMemberDto,
} from '../models/project-member.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectMemberApiService {
  private apiUrl = 'https://localhost:44313/api/projects';

  constructor(private http: HttpClient) {}

  getAll(projectId: string): Observable<ProjectMemberDto[]> {
    return this.http.get<ProjectMemberDto[]>(
      `${this.apiUrl}/${projectId}/members`
    );
  }

  create(
    projectId: string,
    member: AddProjectMemberInput
  ): Observable<ProjectMemberDto> {
    return this.http.post<ProjectMemberDto>(
      `${this.apiUrl}/${projectId}/members`,
      member
    );
  }

  delete(projectId: string, userId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${projectId}/members/${userId}`
    );
  }
}
