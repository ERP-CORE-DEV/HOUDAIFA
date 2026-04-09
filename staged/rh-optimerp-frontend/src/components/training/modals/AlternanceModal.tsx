import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, DatePicker, message } from 'antd';
import dayjs from 'dayjs';
import { alternanceService } from '../../../services/training';
import {
  AlternanceContract,
  AlternanceContractType,
} from '../../../services/training/alternanceService';

const { Option } = Select;

interface AlternanceModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: AlternanceContract | null;
}

type AlternanceFormValues = {
  ApprentiId: string;
  ApprentiNom: string;
  ApprentiAge: number;
  MaitreApprentissageId: string;
  MaitreApprentissageNom: string;
  ContractType: AlternanceContractType;
  StartDate: dayjs.Dayjs;
  EndDate: dayjs.Dayjs;
  Remuneration: number;
  CfaId?: string;
  CfaNom?: string;
};

const CONTRACT_TYPE_OPTIONS: { value: AlternanceContractType; label: string }[] = [
  { value: 'Apprentissage', label: 'Contrat d\'apprentissage' },
  { value: 'Professionnalisation', label: 'Contrat de professionnalisation' },
];

const SMIC_HORAIRE = 11.88;
const SMIC_MENSUEL_BASE = Math.round(SMIC_HORAIRE * 151.67);

const AlternanceModal: React.FC<AlternanceModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<AlternanceFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        ApprentiId: editRecord.ApprentiId,
        ApprentiNom: editRecord.ApprentiNom,
        ApprentiAge: editRecord.ApprentiAge,
        MaitreApprentissageId: editRecord.MaitreApprentissageId,
        MaitreApprentissageNom: editRecord.MaitreApprentissageNom,
        ContractType: editRecord.ContractType,
        StartDate: dayjs(editRecord.StartDate),
        EndDate: dayjs(editRecord.EndDate),
        Remuneration: editRecord.Remuneration,
        CfaId: editRecord.CfaId,
        CfaNom: editRecord.CfaNom,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ Remuneration: SMIC_MENSUEL_BASE });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      const payload: Partial<AlternanceContract> = {
        ApprentiId: values.ApprentiId,
        ApprentiNom: values.ApprentiNom,
        ApprentiAge: values.ApprentiAge,
        MaitreApprentissageId: values.MaitreApprentissageId,
        MaitreApprentissageNom: values.MaitreApprentissageNom,
        ContractType: values.ContractType,
        StartDate: values.StartDate.toISOString(),
        EndDate: values.EndDate.toISOString(),
        Remuneration: values.Remuneration,
        CfaId: values.CfaId,
        CfaNom: values.CfaNom,
        IsActive: true,
        Status: editRecord?.Status ?? 'EnCours',
      };
      if (editRecord) {
        await alternanceService.update(editRecord.Id, payload);
        message.success("Contrat d'alternance mis a jour");
      } else {
        await alternanceService.create(payload);
        message.success("Contrat d'alternance cree");
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde du contrat d'alternance");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord
    ? "Modifier le contrat d'alternance"
    : "Nouveau contrat d'alternance";

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Creer'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={620}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="ContractType"
          label="Type de contrat"
          rules={[{ required: true, message: 'Le type de contrat est obligatoire' }]}
        >
          <Select placeholder="Selectionner le type de contrat">
            {CONTRACT_TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="ApprentiNom"
          label="Nom de l'apprenti(e)"
          rules={[{ required: true, message: "Le nom de l'apprenti est obligatoire" }]}
        >
          <Input placeholder="Nom complet de l'apprenti(e)" />
        </Form.Item>

        <Form.Item
          name="ApprentiId"
          label="Identifiant employe apprenti(e)"
          rules={[{ required: true, message: "L'identifiant employe est obligatoire" }]}
        >
          <Input placeholder="ID de l'employe" />
        </Form.Item>

        <Form.Item
          name="ApprentiAge"
          label="Age de l'apprenti(e)"
          rules={[
            { required: true, message: "L'age est obligatoire" },
            { type: 'number', min: 16, max: 29, message: "L'age doit etre compris entre 16 et 29 ans (regime general)" },
          ]}
        >
          <InputNumber style={{ width: '100%' }} placeholder="Ex. : 22" min={16} max={29} />
        </Form.Item>

        <Form.Item
          name="MaitreApprentissageNom"
          label="Nom du maitre d'apprentissage"
          rules={[{ required: true, message: "Le nom du maitre d'apprentissage est obligatoire" }]}
        >
          <Input placeholder="Nom complet du maitre d'apprentissage" />
        </Form.Item>

        <Form.Item
          name="MaitreApprentissageId"
          label="Identifiant maitre d'apprentissage"
          rules={[{ required: true, message: "L'identifiant est obligatoire" }]}
        >
          <Input placeholder="ID du maitre d'apprentissage" />
        </Form.Item>

        <Form.Item
          name="StartDate"
          label="Date de debut du contrat"
          rules={[{ required: true, message: 'La date de debut est obligatoire' }]}
        >
          <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" placeholder="Date de debut" />
        </Form.Item>

        <Form.Item
          name="EndDate"
          label="Date de fin du contrat"
          rules={[{ required: true, message: 'La date de fin est obligatoire' }]}
        >
          <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" placeholder="Date de fin" />
        </Form.Item>

        <Form.Item
          name="Remuneration"
          label="Remuneration mensuelle brute (EUR)"
          rules={[
            { required: true, message: 'La remuneration est obligatoire' },
            { type: 'number', min: 0, message: 'La remuneration doit etre positive' },
          ]}
          extra={`SMIC mensuel de reference : ${SMIC_MENSUEL_BASE} EUR brut`}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder={`Ex. : ${SMIC_MENSUEL_BASE}`}
            min={0}
            step={50}
            formatter={(val) => `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ' ')}
          />
        </Form.Item>

        <Form.Item name="CfaNom" label="Centre de Formation des Apprentis (CFA)">
          <Input placeholder="Nom du CFA" />
        </Form.Item>

        <Form.Item name="CfaId" label="Identifiant CFA">
          <Input placeholder="ID ou code du CFA" />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default AlternanceModal;
