/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { MockListComponent } from './mock-list.component';

describe('MockListComponent', () => {
  let component: MockListComponent;
  let fixture: ComponentFixture<MockListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MockListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MockListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
