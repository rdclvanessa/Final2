using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuffteksWebsite.Models;

namespace BuffteksWebsite.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly BuffteksWebsiteContext _context;

        public ProjectsController(BuffteksWebsiteContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .SingleOrDefaultAsync(m => m.ID == id);

            if (project == null)
            {
                return NotFound();
            }

            var clients = 
                from participant in _context.Clients
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID
                select participant;

            var members = 
                from participant in _context.Members
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID                
                select participant;

            ProjectDetailViewModel pdvm = new ProjectDetailViewModel
            {
                TheProject = project,
                ProjectClients = clients.ToList() ?? null,
                ProjectMembers = members.ToList() ?? null
            };


            return View(pdvm);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProjectName,ProjectDescription")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,ProjectName,ProjectDescription")] Project project)
        {
            if (id != project.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/EditProjectParticipants/5
        public async Task<IActionResult> EditProjectParticipants(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }


            //var clients = await _context.Clients.ToListAsync();

            //CLIENTS
            //pull 'em into lists first
            var clients = await _context.Clients.ToListAsync();
            var projectroster = await _context.ProjectRoster.ToListAsync();

            /*
            var uniqueclients = 
                from participant in clients
                join projectparticipant in projectroster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where participant.ID != projectparticipant.ProjectParticipantID
                select participant;
            */                

            List<SelectListItem> clientsSelectList = new List<SelectListItem>();

            foreach(var client in clients)
            {
                clientsSelectList.Add(new SelectListItem { Value=client.ID, Text = client.FirstName + " " + client.LastName});
            }

            //MEMBERS
            //pull 'em into lists first
            var members = await _context.Members.ToListAsync();

            /*
            var uniquemembers = 
                from participant in members
                join projectparticipant in projectroster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where participant.ID != projectparticipant.ProjectParticipantID
                select participant;    
            */                

            List<SelectListItem> membersSelectList = new List<SelectListItem>();

            foreach(var member in members)
            {
                membersSelectList.Add(new SelectListItem { Value=member.ID, Text = member.FirstName + " " + member.LastName});
            }

            //create and prepare ViewModel
            EditProjectDetailViewModel epdvm = new EditProjectDetailViewModel
            {
                TheProject = project,
                ProjectClientsList = clientsSelectList,
                ProjectMembersList = membersSelectList
            };
            

            return View(epdvm);
        }

        //POST: Projects/AddProjectClientParticpant/x
        [HttpPost]
        public async Task<IActionResult> AddProjectClientParticipant(EditProjectDetailViewModel model)
        {

            if(ModelState.IsValid)
            {

                //ViewData["theid"] = model.ProjectID;

                try{
                    var ProjectId = model.ProjectID;
                    var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == ProjectId);
                    var ClientId = model.SelectedID;
                    var client = await _context.Clients.SingleOrDefaultAsync(m => m.ID == ClientId);

                    ProjectRoster participant = new ProjectRoster { ProjectID = ProjectId, 
                                                                    Project = project, 
                                                                    ProjectParticipantID = ClientId,
                                                                    ProjectParticipant = client };

                    _context.ProjectRoster.Add(participant);
                    _context.SaveChanges();                            
                }
                catch(Exception exp)
                {
                    throw(exp);
                }

                return RedirectToAction(nameof(Details), new { id = model.ProjectID });

            }

             return View();
        }

        //AddProjectMemberParticipant
        //POST: Projects/AddProjectMemberParticipant/
        [HttpPost]        
        public async Task<IActionResult> AddProjectMemberParticipant(EditProjectDetailViewModel model)
        {

            if(ModelState.IsValid)
            {
                try{
                    var ProjectId = model.ProjectID;
                    var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == ProjectId);
                    var MemberId = model.SelectedID;
                    var member = await _context.Members.SingleOrDefaultAsync(m => m.ID == MemberId);

                    ProjectRoster participant = new ProjectRoster { ProjectID = ProjectId, 
                                                                   Project = project, 
                                                                   ProjectParticipantID = MemberId,
                                                                   ProjectParticipant = member };

                    _context.ProjectRoster.Add(participant);
                    _context.SaveChanges();                            
                }
                catch(Exception exp)
                {
                    throw(exp);
                }

                return RedirectToAction(nameof(Details), new { id = model.ProjectID });

            }

             return View();
        }

        public async Task<IActionResult> DeleteProjectParticipant(string pid, string id)
        {
            //DeleteProjectParticipant
           //var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
           var projectparticipant = await _context.ProjectRoster.SingleOrDefaultAsync(m => m.ProjectID == pid && m.ProjectParticipantID == id );
            _context.ProjectRoster.Remove(projectparticipant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = pid });

        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            _context.Projects.Remove(project);

            //also cascade the delete to the ProjectRoster entries associated with this project
            var roster = _context.ProjectRoster.Where(m => m.ProjectID == id);
            List<ProjectRoster> rosterlist = roster.ToList<ProjectRoster>();
            foreach(var person in rosterlist)
            {
                _context.ProjectRoster.Remove(person);
            }
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ID == id);
        }
    }
}
