import { TestBed } from '@angular/core/testing';
import { DialComponent } from './dial.component';

describe('DialComponent', () => {
  function create(filled: number): DialComponent {
    const fixture = TestBed.createComponent(DialComponent);
    fixture.componentRef.setInput('filled', filled);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('lights the logged segments lime and the current segment develop-green', () => {
    const dial = create(2);
    const segments = dial.segments();

    expect(segments[0].color).toBe('var(--lime)');
    expect(segments[1].color).toBe('var(--lime)');
    expect(segments[2].color).toBe('var(--develop)');
    expect(segments[3].color).toBe('var(--ink-line)');
    expect(segments[4].color).toBe('var(--ink-line)');
  });

  it('lights every segment when the loop is complete', () => {
    const dial = create(5);
    expect(dial.segments().every((s) => s.color === 'var(--lime)')).toBe(true);
  });

  it('builds an accessible label listing completed movements', () => {
    expect(create(2).ariaLabel()).toBe('2 of 5 movements complete: Request, Receive');
  });
});
