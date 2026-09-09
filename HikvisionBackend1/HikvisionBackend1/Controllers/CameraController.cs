using HikvisionBackend1.DTOs;
using HikvisionBackend1.Interfaces;
using HikvisionBackend1.Models;
using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly ICamera Camera;

        public CameraController(ICamera _Camera)
        {
            Camera = _Camera;
        }


        // =========================
        // ADD CAMERA
        // POST: api/Camera/add
        // =========================

        [HttpPost("add")]
        public async Task<ResponceDTO> AddCamera(
            Camera Model)
        {
            return await Camera.AddCamera(Model);
        }


        // =========================
        // GET ALL CAMERAS
        // GET: api/Camera/all
        // =========================

        [HttpGet("all")]
        public async Task<ResponceDTO> GetAllCameras()
        {
            return await Camera.GetAllCameras();
        }


        // =========================
        // GET CAMERA BY ID
        // GET: api/Camera/1
        // =========================

        [HttpGet("{id}")]
        public async Task<ResponceDTO> GetCameraById(
            int id)
        {
            return await Camera.GetCameraById(id);
        }


        // =========================
        // UPDATE CAMERA
        // PUT: api/Camera/update/1
        // =========================

        [HttpPut("update/{id}")]
        public async Task<ResponceDTO> UpdateCamera(
            int id,
            Camera Model)
        {
            return await Camera.UpdateCamera(
                id,
                Model
            );
        }


        // =========================
        // DELETE CAMERA
        // DELETE: api/Camera/delete/1
        // =========================

        [HttpDelete("delete/{id}")]
        public async Task<ResponceDTO> DeleteCamera(
            int id)
        {
            return await Camera.DeleteCamera(id);
        }
    }
}