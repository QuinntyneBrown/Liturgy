import { Routes } from '@angular/router';
import { authGuard } from '@liturgy/api';
import { ShellComponent } from './shell/shell.component';
import { LandingComponent } from './pages/landing/landing.component';
import { DesignSystemComponent } from './pages/design-system/design-system.component';
import { SignInComponent } from './pages/sign-in/sign-in.component';
import { SignUpComponent } from './pages/sign-up/sign-up.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ProjectsComponent } from './pages/projects/projects.component';
import { MembersComponent } from './pages/members/members.component';
import { ProjectJourneyComponent } from './pages/project-journey/project-journey.component';
import { DiscernComponent } from './pages/discern/discern.component';
import { DevelopBoardComponent } from './pages/develop-board/develop-board.component';
import { LoopComponent } from './pages/loop/loop.component';
import { DemonstrateComponent } from './pages/demonstrate/demonstrate.component';

export const routes: Routes = [
  // Public
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'design-system', component: DesignSystemComponent },
  { path: 'sign-in', component: SignInComponent },
  { path: 'sign-up', component: SignUpComponent },

  // Authenticated app
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'projects', component: ProjectsComponent },
      { path: 'members', component: MembersComponent },
      { path: 'projects/:id', component: ProjectJourneyComponent },
      { path: 'discern/:projectId', component: DiscernComponent },
      { path: 'board/:projectId', component: DevelopBoardComponent },
      { path: 'loop/:cardId', component: LoopComponent },
      { path: 'demonstrate/:projectId', component: DemonstrateComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];
