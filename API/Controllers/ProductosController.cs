﻿using API.Dtos;
using API.Helpers;
using API.Helpers.Errors;
using Asp.Versioning;
using AutoMapper;
using CORE.Entities;
using CORE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiVersion("1.1")]
[Authorize(Roles = "Administrador")]
public class ProductosController : BaseApiController
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly ILogger<ProductosController> _logger;

	public ProductosController(IUnitOfWork unitOfWork, IMapper mapper,
											ILogger<ProductosController> logger)
	{
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_logger = logger;
		_logger.LogInformation("Ejecutando productos");
	}

	/// <summary>
	/// Version 1 del método que devuelve todos los productos páginados y permite
	/// buscar productos específicos
	/// </summary>
	/// <param name="productParams">Es una clase que permite enviar argumentos
	/// al método para la paginación</param>
	/// <returns>Pager<ProductoListDTO></returns>
	[HttpGet]
	[MapToApiVersion("1.0")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Pager<ProductoListDTO>>>Get([FromQuery] Params productParams)
	{
		(int totalRegistros, IEnumerable<Producto> registros) = await _unitOfWork
			.Productos
			.GetAllAsync(productParams.PageIndex, productParams.PageSize, 
																productParams.Search);

		List<ProductoListDTO> listaProductosDTO =
						_mapper.Map<List<ProductoListDTO>>(registros);

		Response.Headers.Append("X-InlineCount", totalRegistros.ToString());

		return new Pager<ProductoListDTO>(productParams.PageIndex, productParams.PageSize,
			totalRegistros, listaProductosDTO, productParams.Search);
	}

	/// <summary>
	/// Este método devuelve todos los productos de la base de datos sin paginar
	/// </summary>
	/// <returns>IEnumerable<ProductoDTO></returns>
	[HttpGet]
	[MapToApiVersion("1.1")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<IEnumerable<ProductoDTO>>> Get11()
	{
		IEnumerable<Producto> productos = await _unitOfWork.Productos.GetAllAsync();
		if (productos is null)
		{
			return NotFound(new ApiResponse(404, "Los productos solicitados no existen"));
		}

		return _mapper.Map<List<ProductoDTO>>(productos);
	}

	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProductoDTO>> Get(int id)
	{
		Producto producto = await _unitOfWork.Productos.GetByIdAsync(id);
		if (producto is null)
		{
			return NotFound(new ApiResponse(404, "El producto solicitado no existe"));
		}

		return _mapper.Map<ProductoDTO>(producto);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Producto>> Post(ProductoAddUpdateDTO productoDTO)
	{
		Producto producto = _mapper.Map<Producto>(productoDTO);

		_unitOfWork.Productos.Add(producto);
		await _unitOfWork.SaveAsync();
		if (producto is null)
		{
			return BadRequest(new ApiResponse(400));
		}

		productoDTO.Id = producto.Id;
		return CreatedAtAction(nameof(Post), new { id = productoDTO.Id }, productoDTO);
	}

	[HttpPut("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProductoAddUpdateDTO>>
							Put(int id, [FromBody]ProductoAddUpdateDTO productoDTO)
	{
		if (productoDTO is null)
		{
			return NotFound(new ApiResponse(404, "El producto solicitado no existe"));
		}

		Producto productoDB = await _unitOfWork.Productos.GetByIdAsync(id);
		if (productoDB is null)
		{
			return NotFound(new ApiResponse(404, "El producto solicitado no existe"));
		}

		Producto producto = _mapper.Map<Producto>(productoDTO);
		_unitOfWork.Productos.Update(producto);
		await _unitOfWork.SaveAsync();

		return productoDTO;
	}

	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Delete(int id)
	{
		Producto producto = await _unitOfWork.Productos.GetByIdAsync(id);
		if (producto is null)
		{
			return NotFound(new ApiResponse(404, "El producto solicitado no existe"));
		}

		_unitOfWork.Productos.Remove(producto);
		await _unitOfWork.SaveAsync();

		return NoContent();
	}
}
