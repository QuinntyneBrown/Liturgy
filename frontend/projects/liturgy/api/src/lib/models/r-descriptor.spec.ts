// Traces to: L2-UX-004
import { describeR } from './r-descriptor';

describe('describeR', () => {
  it('carries the poetic movement titles from the mocks', () => {
    expect(describeR('Render').poeticName).toBe('Build toward what you saw');
    expect(describeR('Rejoice').poeticName).toBe('Give thanks for what was made');
  });

  it('keeps the R name distinct from the poetic title', () => {
    const render = describeR('Render');
    expect(render.name).toBe('Render');
    expect(render.name).not.toBe(render.poeticName);
  });
});
