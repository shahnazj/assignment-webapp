using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models;

public class ProjectsViewModel
{

    public IEnumerable<ProjectViewModel> Projects { get; set; } = [];

    public AddProjectViewModel AddProjectFormData { get; set; } = new();

    public EditProjectViewModel EditProjectFormData { get; set; } = new();

}


