import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GroupData } from '../interfaces/groupData';
import { UserSuggestion } from '../interfaces/userSuggestion';
import { AddNewMemberRequest } from '../interfaces/addNewMemberRequest';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private apiUrl = `${environment.apiUrl}/api/group`;

  constructor(private http: HttpClient) { }

  getGroupDetails(groupId: string): Observable<GroupData> {
    return this.http.get<GroupData>(`${this.apiUrl}/detail/${groupId}`);
  }

  searchUsersNotInGroup(groupId: string, keyword: string): Observable<UserSuggestion[]> {
    return this.http.get<UserSuggestion[]>(`${this.apiUrl}/getUsersToAdd/${groupId}?keyword=${keyword}`);
  }

  addMembers(groupId: string, userIds: string[]): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/addMembers/${groupId}`, { UserIds: userIds });
  }

  removeMember(groupId: string, groupMemberId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/removeMember/${groupId}/${groupMemberId}`, {});
  }

  makeAdmin(groupId: string, groupMemberId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/makeAdmin/${groupId}/${groupMemberId}`, {});
  }

  updateGroup(groupId: string, formData: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${groupId}`, formData);
  }

  deleteGroup(groupId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${groupId}`);
  }
}
