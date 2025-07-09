import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddProjectMemberInput,
  ProjectMemberDto,
} from '../models/project-member.model';
import { AppConfigService } from '../../app/shared/app-config.service';

@Injectable()
export class ProjectMemberApiService {
  private readonly apiUrl;

  constructor(private http: HttpClient,
    private config: AppConfigService
  ) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/projects`;
  }

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
