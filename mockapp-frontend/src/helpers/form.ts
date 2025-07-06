import { FormControl } from "@angular/forms";

export type TypedFormControls<T> = {
  [K in keyof T]: FormControl<T[K]>;
};