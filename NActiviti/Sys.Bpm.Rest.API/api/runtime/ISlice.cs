using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.events.integration;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.services.connectors.model;
using org.springframework.messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.api.runtime.shared.query
{
    public interface ISlice<T> : IEnumerable<T>
    {

    }
}
