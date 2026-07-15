import { Routes } from '@angular/router';
import { authGuard } from '@liturgy/api';
import { ShellComponent } from './shell/shell.component';
import { SignInComponent } from './pages/sign-in/sign-in.component';
import { SignUpComponent } from './pages/sign-up/sign-up.component';
import { ProjectsComponent } from './pages/projects/projects.component';
import { ProjectJourneyComponent } from './pages/project-journey/project-journey.component';
import { DevelopBoardComponent } from './pages/develop-board/develop-board.component';
import { LoopComponent } from './pages/loop/loop.component';

export const routes: Routes = [
  { path: 'sign-in', component: SignInComponent },
  { path: 'sign-up', component: SignUpComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'projects', pathMatch: 'full' },
      { path: 'projects', component: ProjectsComponent },
      { path: 'projects/:id', component: ProjectJourneyComponent },
      { path: 'board/:projectId', component: DevelopBoardComponent },
      { path: 'loop/:cardId', component: LoopComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];
