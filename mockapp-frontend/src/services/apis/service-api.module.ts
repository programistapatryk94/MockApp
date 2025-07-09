import { NgModule } from '@angular/core';
import { MockApiService } from './mock-api.service';
import { ProjectMemberApiService } from './project-member-api.service';
import { ProjectApiService } from './project-api.service';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { TokenInterceptor } from '../../helpers/token.interceptor';

@NgModule({
  providers: [
    MockApiService,
    ProjectApiService,
    ProjectMemberApiService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true,
    },
  ],
})
export class ServiceApiModule {}
