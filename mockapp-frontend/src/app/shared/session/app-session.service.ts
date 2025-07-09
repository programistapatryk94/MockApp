import { Injectable } from '@angular/core';

@Injectable()
export class AppSessionService {
  init() {
    return new Promise<boolean>((resolve, reject) => {
      resolve(true);
    });
  }
}
