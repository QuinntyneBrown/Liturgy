import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DialComponent, PipStripComponent } from '@liturgy/components';

@Component({
  selector: 'lit-design-system',
  imports: [RouterLink, DialComponent, PipStripComponent],
  templateUrl: './design-system.component.html',
  styleUrl: './design-system.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DesignSystemComponent {}
