using HikvisionBackend1.Models;
using HikvisionBackend1.Services;
using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisteredVehicleController : ControllerBase
    {
        private readonly RegisteredVehicleService _service;

        public RegisteredVehicleController(
            RegisteredVehicleService service)
        {
            _service = service;
        }


        // ============================================================
        // GET ALL
        // GET /api/RegisteredVehicle
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vehicles =
                    await _service.GetAllAsync();

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get registered vehicles",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // GET BY CAMERA
        //
        // Front Gate:
        // GET /api/RegisteredVehicle/camera/1
        //
        // Back Gate:
        // GET /api/RegisteredVehicle/camera/2
        // ============================================================

        [HttpGet("camera/{cameraId:int}")]
        public async Task<IActionResult> GetByCamera(
            int cameraId)
        {
            try
            {
                var vehicles =
                    await _service.GetByCameraIdAsync(
                        cameraId);

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get registered vehicles for camera",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // GET BY ID
        //
        // GET /api/RegisteredVehicle/1
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            try
            {
                var vehicle =
                    await _service.GetByIdAsync(id);

                if (vehicle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Registered vehicle not found"
                    });
                }

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get registered vehicle",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // CREATE
        //
        // POST /api/RegisteredVehicle
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] RegisteredVehicle vehicle)
        {
            try
            {
                // Plate number
                if (string.IsNullOrWhiteSpace(
                    vehicle.PlateNumber))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Plate number is required"
                    });
                }


                // Camera
                if (vehicle.CameraId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Camera ID is required"
                    });
                }


                // Vehicle type
                if (string.IsNullOrWhiteSpace(
                    vehicle.VehicleType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Vehicle type is required"
                    });
                }


                // Pass type
                if (vehicle.PassType != "OneTime" &&
                    vehicle.PassType != "AllTime")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "PassType must be OneTime or AllTime"
                    });
                }


                var result =
                    await _service.CreateAsync(
                        vehicle);


                return Ok(new
                {
                    success = true,

                    message =
                        "Vehicle registered successfully",

                    vehicle = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,

                    message =
                        "Unable to register vehicle",

                    error = ex.Message
                });
            }
        }


        // ============================================================
        // UPDATE
        //
        // PUT /api/RegisteredVehicle/1
        // ============================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] RegisteredVehicle vehicle)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    vehicle.PlateNumber))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Plate number is required"
                    });
                }


                if (vehicle.CameraId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Camera ID is required"
                    });
                }


                var result =
                    await _service.UpdateAsync(
                        id,
                        vehicle);


                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Registered vehicle not found"
                    });
                }


                return Ok(new
                {
                    success = true,
                    message =
                        "Vehicle updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to update vehicle",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // ACTIVATE
        //
        // PUT /api/RegisteredVehicle/1/activate
        // ============================================================

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(
            int id)
        {
            try
            {
                var result =
                    await _service.ActivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Registered vehicle not found"
                    });
                }


                return Ok(new
                {
                    success = true,
                    message =
                        "Vehicle activated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to activate vehicle",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // DEACTIVATE
        //
        // PUT /api/RegisteredVehicle/1/deactivate
        // ============================================================

        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(
            int id)
        {
            try
            {
                var result =
                    await _service.DeactivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Registered vehicle not found"
                    });
                }


                return Ok(new
                {
                    success = true,
                    message =
                        "Vehicle deactivated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to deactivate vehicle",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // DELETE
        //
        // DELETE /api/RegisteredVehicle/1
        // ============================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                var result =
                    await _service.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Registered vehicle not found"
                    });
                }


                return Ok(new
                {
                    success = true,
                    message =
                        "Vehicle deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to delete vehicle",
                    error = ex.Message
                });
            }
        }
    }
}