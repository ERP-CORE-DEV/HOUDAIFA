{{/*
Expand the name of the chart.
*/}}
{{- define "training-service.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
Truncated to 63 chars because Kubernetes DNS naming rules require it.
If release name already contains the chart name, it is not duplicated.
*/}}
{{- define "training-service.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart label value: "<chart-name>-<chart-version>"
*/}}
{{- define "training-service.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels applied to every resource.
*/}}
{{- define "training-service.labels" -}}
helm.sh/chart: {{ include "training-service.chart" . }}
{{ include "training-service.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: rh-optimerp
app.kubernetes.io/component: backend
{{- end }}

{{/*
Selector labels used by Deployment and Service to target pods.
These must remain stable across upgrades — do not add mutable fields here.
*/}}
{{- define "training-service.selectorLabels" -}}
app.kubernetes.io/name: {{ include "training-service.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Resolve the ServiceAccount name to use for the Deployment.
*/}}
{{- define "training-service.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "training-service.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
