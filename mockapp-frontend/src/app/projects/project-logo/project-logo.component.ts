import { Component, Input, OnInit } from '@angular/core';
import { Project } from '../../../services/models/project.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-project-logo',
  templateUrl: './project-logo.component.html',
  styleUrls: ['./project-logo.component.scss'],
  imports: [CommonModule],
})
export class ProjectLogoComponent implements OnInit {
  @Input()
  public project?: Project;

  constructor() {}

  ngOnInit() {}
}
