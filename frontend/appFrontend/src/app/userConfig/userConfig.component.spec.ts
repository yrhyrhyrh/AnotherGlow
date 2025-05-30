import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserConfigComponent } from './userConfig.component';

describe('HomeComponent', () => {
  let component: UserConfigComponent;
  let fixture: ComponentFixture<UserConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserConfigComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
