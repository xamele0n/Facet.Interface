# Facet Roadmap

This document outlines features and enhancements that could be added to Facet.

### 1.1.x (Current)

- Property and field inclusion/exclusion
- Constructor generation
- Custom mapping via IFacetMapConfiguration
- Full support for nested types
- Fully qualified type names to avoid import issues

### 1.2 onwards

#### Nested Facet Projections

Automatically generate DTOs for nested types when they are also marked with [Facet]
Possibly opt-in via: GenerateNested = true

#### Init-only / Readonly Property Mode

Generate init accessors for properties instead of set

#### Add support for readonly fields

Configurable via PropertyMode = InitOnly

#### Interface Generation

Generate an interface (IPersonDto) that matches the generated DTO
Opt-in: `GenerateInterface = true` for example

#### Selective Inclusion ("whitelist")

Add Include argument to attribute for positive selection of members
Mutually exclusive with Exclude

### Exploratory Ideas

#### [FacetFor] Inversion Mode

Apply `[FacetFor(typeof(Target))]` on source model instead of DTO

#### Custom Member Attributes

Allow attaching custom attributes to generated properties/fields via config

#### Record Support

Option to generate record instead of class