import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LanguageService } from './language.service';

@Injectable()
export class LanguageTokenInterceptor implements HttpInterceptor {
  constructor(private language: LanguageService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    var currentLanguage = this.language.currentLocale;
    console.log('this', this.language);
    const langReq = currentLanguage
      ? req.clone({
          setHeaders: {
            '.AspNetCore.Culture': `c=${currentLanguage}|uic=${currentLanguage}`,
          },
        })
      : req;

    return next.handle(langReq);
  }
}
