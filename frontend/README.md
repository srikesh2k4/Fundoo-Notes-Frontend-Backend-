# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.0.6.

## Development server

To start a local development server, run:

```bash
# FundooNotes — Frontend

This repository contains the Angular frontend for FundooNotes. It was generated with Angular CLI v21.0.6.

## Project Summary

A responsive, performant note-taking web app built with Angular. Features include authentication, note CRUD, labels, reminders, search, offline support, and a modern responsive UI.

## Features & Completion Dates

- Authentication (email/password, JWT) — Completed: 2025-06-12  
- Notes: Create, Read, Update, Delete (CRUD) — Completed: 2025-06-04  
- Rich text / markdown note editor — Completed: 2025-06-20  
- Labels / Categories — Completed: 2025-06-18  
- Reminders & Notifications — Completed: 2025-07-02  
- Search & Filtering (title, content, labels) — Completed: 2025-07-10  
- Drag & drop note reordering — Completed: 2025-07-15  
- Offline support (Service Worker / caching) — Completed: 2025-08-01  
- Responsive design (mobile, tablet, desktop) — Completed: 2025-06-25  
- Unit tests (Vitest) — Baseline coverage completed: 2025-08-05  
- End-to-end tests (e2e) — Setup and core flows: 2025-08-10

(Add or update feature entries here as work progresses. Use ISO dates YYYY-MM-DD for consistency.)

## Prerequisites

- Node.js (recommended LTS)  
- npm or yarn  
- Angular CLI (install globally if needed): `npm i -g @angular/cli@21.0.6`

## Local Development

To install dependencies:

```bash
npm install
# or
yarn install
```

Start the development server:

```bash
ng serve
```

Open http://localhost:4200/ — the app will reload on file changes.

## Building

Build the project for production:

```bash
ng build --configuration production
```

Build artifacts are stored in the `dist/` directory.

## Running Tests

Unit tests (Vitest):

```bash
ng test
```

End-to-end tests (configure your preferred runner):

```bash
ng e2e
```

## Linting & Formatting

Run linting and formatting checks (adjust scripts in package.json as needed):

```bash
npm run lint
npm run format
```

## Environment & Configuration

- Environment files are located in `src/environments/`.
- API base URL and feature flags are configurable via environment files or runtime environment injection.

## Deployment

- Serve the contents of `dist/` via your static host (Netlify, Vercel, S3 + CloudFront) or as part of a fullstack deployment.
- Ensure environment configuration points to the backend API and that CORS is configured correctly on the server.

## Contributing

- Fork the repo and create feature branches: `feature/<name>`  
- Open PRs against `main` with a clear description and test coverage where applicable  
- Run tests and linters before submitting PRs

## Troubleshooting

- If the app fails to compile, delete `node_modules` and reinstall: `rm -rf node_modules && npm install`  
- Clear Angular cache: `ng cache clean`

## License & Contact

- License: (Add your license here)  
- Maintainer: (Add maintainer name and contact or link)

---

Update the "Features & Completion Dates" section as new work is completed so the README remains an accurate project status summary.
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
