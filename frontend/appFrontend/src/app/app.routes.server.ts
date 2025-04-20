import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'group/detail/:id',
    renderMode: RenderMode.Client  // <- skip prerendering for this
  },
  {
    path: 'groups/:groupId/posts',
    renderMode: RenderMode.Client  // <- skip prerendering for this
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender  // everything else gets prerendered
  }
];
