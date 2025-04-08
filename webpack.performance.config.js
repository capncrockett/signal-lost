const path = require('path');

module.exports = {
  mode: 'production',
  entry: {
    'performance-test-bundle': './tests/performance/browser-bundle.ts',
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'dist'),
    library: {
      type: 'umd',
      name: 'PerformanceTest',
    },
  },
  resolve: {
    extensions: ['.ts', '.js'],
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
};
