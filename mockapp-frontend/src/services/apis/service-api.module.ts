import { NgModule } from '@angular/core';
import { MockApiService } from './mock-api.service';
import { ProjectMemberApiService } from './project-member-api.service';
import { ProjectApiService } from './project-api.service';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { TokenInterceptor } from '../../helpers/token.interceptor';
import { SubscriptionApiService } from './subscription-api.service';
import { LanguageTokenInterceptor } from '../../helpers/language-token.interceptor';
import { ErrorInterceptor } from '../../helpers/error.interceptor';

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
    {
      provide: HTTP_INTERCEPTORS,
      useClass: LanguageTokenInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true,
    },
  ],
})
export class ServiceApiModule {}
