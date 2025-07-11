import { NgModule } from '@angular/core';
import { MockApiService } from './mock-api.service';
import { ProjectMemberApiService } from './project-member-api.service';
import { ProjectApiService } from './project-api.service';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { TokenInterceptor } from '../../helpers/token.interceptor';
import { SubscriptionApiService } from './subscription-api.service';

@NgModule({
  providers: [
    MockApiService,
    ProjectApiService,
    ProjectMemberApiService,
    SubscriptionApiService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true,
    },
  ],
})
export class ServiceApiModule {}
