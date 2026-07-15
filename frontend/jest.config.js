const { createCjsPreset } = require('jest-preset-angular/presets');

/** @type {import('jest').Config} */
module.exports = {
  ...createCjsPreset(),
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
  moduleNameMapper: {
    '^@liturgy/api$': '<rootDir>/projects/liturgy/api/src/public-api.ts',
    '^@liturgy/components$': '<rootDir>/projects/liturgy/components/src/public-api.ts',
    '^@liturgy/domain$': '<rootDir>/projects/liturgy/domain/src/public-api.ts',
  },
  testMatch: ['<rootDir>/projects/**/*.spec.ts'],
  testPathIgnorePatterns: ['<rootDir>/node_modules/', '<rootDir>/dist/', '<rootDir>/e2e/'],
};
