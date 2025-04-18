import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../shared/sidebar/sidebar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SidebarComponent
  ],
  template: `
    <div class="header-container">
      <app-header />
    </div>
    <div class="content-wrapper">
      <app-sidebar />
      <main class="main-content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
    }

    .header-container {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 1001;
      background-color: white;
    }

    .content-wrapper {
      display: flex;
      padding-top: 64px; /* Height of the header */
      min-height: calc(100vh - 64px);
    }

    .main-content {
      flex: 1;
      margin-left: 280px; /* Width of the sidebar */
      padding: 20px;
      background-color: #f5f5f5;
      min-height: calc(100vh - 64px);
    }
  `]
})
export class MainLayoutComponent { } 