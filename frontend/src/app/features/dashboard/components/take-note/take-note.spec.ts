import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TakeNote } from './take-note';

describe('TakeNote', () => {
  let component: TakeNote;
  let fixture: ComponentFixture<TakeNote>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TakeNote]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TakeNote);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
