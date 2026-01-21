import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoteCardComponent } from './note-card';  // ✅ Fixed import

describe('NoteCardComponent', () => {  // ✅ Fixed name
  let component: NoteCardComponent;  // ✅ Fixed type
  let fixture: ComponentFixture<NoteCardComponent>;  // ✅ Fixed type

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoteCardComponent]  // ✅ Fixed import
    })
    .compileComponents();

    fixture = TestBed.createComponent(NoteCardComponent);  // ✅ Fixed
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
