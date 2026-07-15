import { R_ORDER } from './r-kind';

describe('R_ORDER', () => {
  it('lists the five Rs in loop order', () => {
    expect(R_ORDER).toEqual(['Request', 'Receive', 'Review', 'Render', 'Rejoice']);
  });
});
