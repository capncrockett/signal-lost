/**
 * Contract Validation Script
 * 
 * This script validates the interface contracts between Alpha and Beta agents.
 * It checks for compatibility issues and ensures that both agents can work together.
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Define the paths to check
const typesDir = path.join(__dirname, '..', 'src', 'types');
const alphaComponentsDir = path.join(__dirname, '..', 'src', 'components', 'radio');
const betaComponentsDir = path.join(__dirname, '..', 'src', 'components', 'narrative');

// Check if directories exist
if (!fs.existsSync(typesDir)) {
  console.error('Types directory not found');
  process.exit(1);
}

// Required interfaces
const requiredInterfaces = [
  'Signal',
  'GameEvent'
];

// Check for required interfaces
console.log('## Checking for required interfaces');
let interfacesFound = 0;

// Read all .d.ts files in the types directory
const typeFiles = fs.readdirSync(typesDir).filter(file => file.endsWith('.d.ts'));

for (const file of typeFiles) {
  const content = fs.readFileSync(path.join(typesDir, file), 'utf8');
  
  for (const interfaceName of requiredInterfaces) {
    if (content.includes(`interface ${interfaceName}`)) {
      console.log(`✅ ${interfaceName} interface found in ${file}`);
      interfacesFound++;
    }
  }
}

if (interfacesFound < requiredInterfaces.length) {
  console.error('❌ Not all required interfaces were found');
  process.exit(1);
}

// Check for Alpha/Beta component compatibility
console.log('\n## Checking Alpha/Beta component compatibility');

// Check if Alpha components reference Beta interfaces correctly
if (fs.existsSync(alphaComponentsDir)) {
  console.log('\n### Alpha Components');
  const alphaFiles = fs.readdirSync(alphaComponentsDir).filter(file => 
    file.endsWith('.tsx') || file.endsWith('.ts')
  );
  
  for (const file of alphaFiles) {
    const content = fs.readFileSync(path.join(alphaComponentsDir, file), 'utf8');
    
    // Check for proper imports
    if (content.includes('import') && content.includes('narrative')) {
      console.log(`✅ ${file} correctly imports from narrative components`);
    }
    
    // Check for signal handling
    if (content.includes('Signal') || content.includes('signal')) {
      console.log(`✅ ${file} handles signals`);
    }
  }
}

// Check if Beta components reference Alpha interfaces correctly
if (fs.existsSync(betaComponentsDir)) {
  console.log('\n### Beta Components');
  const betaFiles = fs.readdirSync(betaComponentsDir).filter(file => 
    file.endsWith('.tsx') || file.endsWith('.ts')
  );
  
  for (const file of betaFiles) {
    const content = fs.readFileSync(path.join(betaComponentsDir, file), 'utf8');
    
    // Check for proper imports
    if (content.includes('import') && content.includes('radio')) {
      console.log(`✅ ${file} correctly imports from radio components`);
    }
    
    // Check for signal handling
    if (content.includes('Signal') || content.includes('signal')) {
      console.log(`✅ ${file} handles signals`);
    }
  }
}

// Run TypeScript compiler to check for type errors
console.log('\n## Running TypeScript compiler checks');
try {
  execSync('npx tsc --noEmit --strict src/types/*.d.ts', { stdio: 'inherit' });
  console.log('✅ TypeScript compiler check passed');
} catch (error) {
  console.error('❌ TypeScript compiler check failed');
  process.exit(1);
}

console.log('\n## Contract validation completed successfully');
process.exit(0);
