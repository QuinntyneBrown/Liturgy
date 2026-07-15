import { TestBed } from '@angular/core/testing';
import { Gate } from '@liturgy/api';
import { GateComponent, RequirementToggle } from './gate.component';

function gate(): Gate {
  return {
    id: 'g1',
    phaseId: 'p1',
    title: 'Develop → Demonstrate',
    state: 'Blocked',
    requirements: [
      { id: 'r1', label: 'One', meta: null, state: 'Done', order: 0 },
      { id: 'r2', label: 'Two', meta: null, state: 'Todo', order: 1 },
    ],
  };
}

describe('GateComponent', () => {
  function create(): GateComponent {
    const fixture = TestBed.createComponent(GateComponent);
    fixture.componentRef.setInput('gate', gate());
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('counts the remaining (not-done) requirements', () => {
    expect(create().remaining()).toBe(1);
  });

  it('emits a toggle that flips a Todo requirement to done', () => {
    const component = create();
    let emitted: RequirementToggle | undefined;
    component.toggle.subscribe((e) => (emitted = e));

    component.onToggle(gate().requirements[1]);

    expect(emitted?.requirement.id).toBe('r2');
    expect(emitted?.done).toBe(true);
  });

  it('labels the blocked badge with the remaining requirement count', () => {
    const component = create();
    expect(component.isOpen()).toBe(false);
    expect(component.remainingLabel()).toBe('1 requirement left');
  });

  it('renders a locked advance control (aria-disabled) while blocked', () => {
    const fixture = TestBed.createComponent(GateComponent);
    fixture.componentRef.setInput('gate', gate());
    fixture.componentRef.setInput('advanceLabel', 'Open Develop');
    fixture.detectChanges();
    const advance = fixture.nativeElement.querySelector('.gate__advance');
    expect(advance).not.toBeNull();
    expect(advance.getAttribute('aria-disabled')).toBe('true');
  });
});
